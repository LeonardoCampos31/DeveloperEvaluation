using AutoMapper;
using MediatR;
using Sales.Application.Commands.Sales;
using Sales.Application.DTOs;
using Sales.Domain.Interfaces;

namespace Sales.Application.Handlers.Sales;

public class GetSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    : IRequestHandler<GetSalesQuery, IEnumerable<SaleDto>>
{
    private readonly ISaleRepository _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<IEnumerable<SaleDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);

        var salesDto = _mapper.Map<IEnumerable<SaleDto>>(sales);

        return salesDto;
    }
}